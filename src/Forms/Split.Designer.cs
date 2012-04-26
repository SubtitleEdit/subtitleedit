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
            this.labelNumberOfParts = new System.Windows.Forms.Label();
            this.numericUpDownParts = new System.Windows.Forms.NumericUpDown();
            this.radioButtonCharacters = new System.Windows.Forms.RadioButton();
            this.RadioButtonLines = new System.Windows.Forms.RadioButton();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.comboBoxEncoding = new System.Windows.Forms.ComboBox();
            this.labelOutputFormat = new System.Windows.Forms.Label();
            this.labelChooseOutputFolder = new System.Windows.Forms.Label();
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.textBoxOutputFolder = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.listViewParts = new System.Windows.Forms.ListView();
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSplit = new System.Windows.Forms.Button();
            this.buttonBasic = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.labelLines = new System.Windows.Forms.Label();
            this.labelCharacters = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.comboBoxSubtitleFormats = new System.Windows.Forms.ComboBox();
            this.groupBoxParts.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownParts)).BeginInit();
            this.groupBoxOutput.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxParts
            // 
            this.groupBoxParts.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxParts.Controls.Add(this.groupBox1);
            this.groupBoxParts.Controls.Add(this.labelNumberOfParts);
            this.groupBoxParts.Controls.Add(this.numericUpDownParts);
            this.groupBoxParts.Controls.Add(this.radioButtonCharacters);
            this.groupBoxParts.Controls.Add(this.RadioButtonLines);
            this.groupBoxParts.Location = new System.Drawing.Point(13, 13);
            this.groupBoxParts.Name = "groupBoxParts";
            this.groupBoxParts.Size = new System.Drawing.Size(606, 86);
            this.groupBoxParts.TabIndex = 0;
            this.groupBoxParts.TabStop = false;
            this.groupBoxParts.Text = "Split options";
            // 
            // labelNumberOfParts
            // 
            this.labelNumberOfParts.AutoSize = true;
            this.labelNumberOfParts.Location = new System.Drawing.Point(148, 22);
            this.labelNumberOfParts.Name = "labelNumberOfParts";
            this.labelNumberOfParts.Size = new System.Drawing.Size(111, 13);
            this.labelNumberOfParts.TabIndex = 3;
            this.labelNumberOfParts.Text = "Number of equal parts";
            // 
            // numericUpDownParts
            // 
            this.numericUpDownParts.Location = new System.Drawing.Point(151, 38);
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
            this.numericUpDownParts.ValueChanged += new System.EventHandler(this.numericUpDownParts_ValueChanged);
            // 
            // radioButtonCharacters
            // 
            this.radioButtonCharacters.AutoSize = true;
            this.radioButtonCharacters.Location = new System.Drawing.Point(15, 43);
            this.radioButtonCharacters.Name = "radioButtonCharacters";
            this.radioButtonCharacters.Size = new System.Drawing.Size(76, 17);
            this.radioButtonCharacters.TabIndex = 1;
            this.radioButtonCharacters.Text = "Characters";
            this.radioButtonCharacters.UseVisualStyleBackColor = true;
            this.radioButtonCharacters.CheckedChanged += new System.EventHandler(this.radioButtonCharacters_CheckedChanged);
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
            this.RadioButtonLines.CheckedChanged += new System.EventHandler(this.RadioButtonLines_CheckedChanged);
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOutput.Controls.Add(this.comboBoxSubtitleFormats);
            this.groupBoxOutput.Controls.Add(this.label1);
            this.groupBoxOutput.Controls.Add(this.comboBoxEncoding);
            this.groupBoxOutput.Controls.Add(this.labelOutputFormat);
            this.groupBoxOutput.Controls.Add(this.labelChooseOutputFolder);
            this.groupBoxOutput.Controls.Add(this.buttonChooseFolder);
            this.groupBoxOutput.Controls.Add(this.textBoxOutputFolder);
            this.groupBoxOutput.Location = new System.Drawing.Point(12, 105);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(610, 106);
            this.groupBoxOutput.TabIndex = 1;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "Output";
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.FormattingEnabled = true;
            this.comboBoxEncoding.Location = new System.Drawing.Point(367, 69);
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.Size = new System.Drawing.Size(221, 21);
            this.comboBoxEncoding.TabIndex = 6;
            // 
            // labelOutputFormat
            // 
            this.labelOutputFormat.AutoSize = true;
            this.labelOutputFormat.Location = new System.Drawing.Point(6, 72);
            this.labelOutputFormat.Name = "labelOutputFormat";
            this.labelOutputFormat.Size = new System.Drawing.Size(39, 13);
            this.labelOutputFormat.TabIndex = 5;
            this.labelOutputFormat.Text = "Format";
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
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.Location = new System.Drawing.Point(566, 39);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(26, 23);
            this.buttonChooseFolder.TabIndex = 1;
            this.buttonChooseFolder.Text = "...";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            this.buttonChooseFolder.Click += new System.EventHandler(this.buttonChooseFolder_Click);
            // 
            // textBoxOutputFolder
            // 
            this.textBoxOutputFolder.Location = new System.Drawing.Point(7, 39);
            this.textBoxOutputFolder.Name = "textBoxOutputFolder";
            this.textBoxOutputFolder.Size = new System.Drawing.Size(553, 20);
            this.textBoxOutputFolder.TabIndex = 0;
            this.textBoxOutputFolder.TextChanged += new System.EventHandler(this.textBoxOutputFolder_TextChanged);
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.listViewParts);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 217);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(610, 148);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Prevew";
            // 
            // listViewParts
            // 
            this.listViewParts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5,
            this.columnHeader6,
            this.columnHeaderFileName});
            this.listViewParts.Dock = System.Windows.Forms.DockStyle.Fill;
            this.listViewParts.FullRowSelect = true;
            this.listViewParts.HideSelection = false;
            this.listViewParts.Location = new System.Drawing.Point(3, 16);
            this.listViewParts.Name = "listViewParts";
            this.listViewParts.Size = new System.Drawing.Size(604, 129);
            this.listViewParts.TabIndex = 101;
            this.listViewParts.UseCompatibleStateImageBehavior = false;
            this.listViewParts.View = System.Windows.Forms.View.Details;
            // 
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "File name";
            this.columnHeaderFileName.Width = 463;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "#Lines";
            this.columnHeader5.Width = 61;
            // 
            // columnHeader6
            // 
            this.columnHeader6.Text = "#Characters";
            this.columnHeader6.Width = 75;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(547, 371);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 4;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSplit
            // 
            this.buttonSplit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSplit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSplit.Location = new System.Drawing.Point(466, 371);
            this.buttonSplit.Name = "buttonSplit";
            this.buttonSplit.Size = new System.Drawing.Size(75, 21);
            this.buttonSplit.TabIndex = 3;
            this.buttonSplit.Text = "&Split";
            this.buttonSplit.UseVisualStyleBackColor = true;
            this.buttonSplit.Click += new System.EventHandler(this.buttonSplit_Click);
            // 
            // buttonBasic
            // 
            this.buttonBasic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBasic.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonBasic.Location = new System.Drawing.Point(362, 371);
            this.buttonBasic.Name = "buttonBasic";
            this.buttonBasic.Size = new System.Drawing.Size(98, 21);
            this.buttonBasic.TabIndex = 24;
            this.buttonBasic.Text = "&Basic";
            this.buttonBasic.UseVisualStyleBackColor = true;
            this.buttonBasic.Click += new System.EventHandler(this.buttonBasic_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.labelCharacters);
            this.groupBox1.Controls.Add(this.labelLines);
            this.groupBox1.Location = new System.Drawing.Point(311, 13);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(289, 67);
            this.groupBox1.TabIndex = 4;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Subtitle info";
            // 
            // labelLines
            // 
            this.labelLines.AutoSize = true;
            this.labelLines.Location = new System.Drawing.Point(7, 27);
            this.labelLines.Name = "labelLines";
            this.labelLines.Size = new System.Drawing.Size(93, 13);
            this.labelLines.TabIndex = 0;
            this.labelLines.Text = "Number of lines: X";
            // 
            // labelCharacters
            // 
            this.labelCharacters.AutoSize = true;
            this.labelCharacters.Location = new System.Drawing.Point(7, 46);
            this.labelCharacters.Name = "labelCharacters";
            this.labelCharacters.Size = new System.Drawing.Size(122, 13);
            this.labelCharacters.TabIndex = 1;
            this.labelCharacters.Text = "Number of characters: X";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(309, 72);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 13);
            this.label1.TabIndex = 7;
            this.label1.Text = "Encoding";
            // 
            // comboBoxSubtitleFormats
            // 
            this.comboBoxSubtitleFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFormats.FormattingEnabled = true;
            this.comboBoxSubtitleFormats.Location = new System.Drawing.Point(51, 69);
            this.comboBoxSubtitleFormats.Name = "comboBoxSubtitleFormats";
            this.comboBoxSubtitleFormats.Size = new System.Drawing.Size(225, 21);
            this.comboBoxSubtitleFormats.TabIndex = 8;
            // 
            // Split
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(634, 403);
            this.Controls.Add(this.buttonBasic);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSplit);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.groupBoxOutput);
            this.Controls.Add(this.groupBoxParts);
            this.MinimumSize = new System.Drawing.Size(640, 420);
            this.Name = "Split";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Split";
            this.Shown += new System.EventHandler(this.Split_Shown);
            this.ResizeEnd += new System.EventHandler(this.Split_ResizeEnd);
            this.groupBoxParts.ResumeLayout(false);
            this.groupBoxParts.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownParts)).EndInit();
            this.groupBoxOutput.ResumeLayout(false);
            this.groupBoxOutput.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxParts;
        private System.Windows.Forms.Label labelNumberOfParts;
        private System.Windows.Forms.NumericUpDown numericUpDownParts;
        private System.Windows.Forms.RadioButton radioButtonCharacters;
        private System.Windows.Forms.RadioButton RadioButtonLines;
        private System.Windows.Forms.GroupBox groupBoxOutput;
        private System.Windows.Forms.ComboBox comboBoxEncoding;
        private System.Windows.Forms.Label labelOutputFormat;
        private System.Windows.Forms.Label labelChooseOutputFolder;
        private System.Windows.Forms.Button buttonChooseFolder;
        private System.Windows.Forms.TextBox textBoxOutputFolder;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonSplit;
        private System.Windows.Forms.ListView listViewParts;
        private System.Windows.Forms.ColumnHeader columnHeaderFileName;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ColumnHeader columnHeader6;
        private System.Windows.Forms.Button buttonBasic;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label labelCharacters;
        private System.Windows.Forms.Label labelLines;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFormats;
        private System.Windows.Forms.Label label1;
    }
}