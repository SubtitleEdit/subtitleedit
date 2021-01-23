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
            this.groupBoxSplitOptions = new System.Windows.Forms.GroupBox();
            this.groupBoxSubtitleInfo = new System.Windows.Forms.GroupBox();
            this.labelCharacters = new System.Windows.Forms.Label();
            this.labelLines = new System.Windows.Forms.Label();
            this.labelNumberOfParts = new System.Windows.Forms.Label();
            this.numericUpDownParts = new System.Windows.Forms.NumericUpDown();
            this.radioButtonCharacters = new System.Windows.Forms.RadioButton();
            this.RadioButtonLines = new System.Windows.Forms.RadioButton();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.buttonOpenOutputFolder = new System.Windows.Forms.Button();
            this.labelFileName = new System.Windows.Forms.Label();
            this.textBoxFileName = new System.Windows.Forms.TextBox();
            this.comboBoxSubtitleFormats = new System.Windows.Forms.ComboBox();
            this.labelEncoding = new System.Windows.Forms.Label();
            this.comboBoxEncoding = new System.Windows.Forms.ComboBox();
            this.labelOutputFormat = new System.Windows.Forms.Label();
            this.labelChooseOutputFolder = new System.Windows.Forms.Label();
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.textBoxOutputFolder = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.listViewParts = new System.Windows.Forms.ListView();
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader6 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFileName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonSplit = new System.Windows.Forms.Button();
            this.buttonBasic = new System.Windows.Forms.Button();
            this.radioButtonTime = new System.Windows.Forms.RadioButton();
            this.groupBoxSplitOptions.SuspendLayout();
            this.groupBoxSubtitleInfo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownParts)).BeginInit();
            this.groupBoxOutput.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxSplitOptions
            // 
            this.groupBoxSplitOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSplitOptions.Controls.Add(this.radioButtonTime);
            this.groupBoxSplitOptions.Controls.Add(this.groupBoxSubtitleInfo);
            this.groupBoxSplitOptions.Controls.Add(this.labelNumberOfParts);
            this.groupBoxSplitOptions.Controls.Add(this.numericUpDownParts);
            this.groupBoxSplitOptions.Controls.Add(this.radioButtonCharacters);
            this.groupBoxSplitOptions.Controls.Add(this.RadioButtonLines);
            this.groupBoxSplitOptions.Location = new System.Drawing.Point(13, 13);
            this.groupBoxSplitOptions.Name = "groupBoxSplitOptions";
            this.groupBoxSplitOptions.Size = new System.Drawing.Size(684, 96);
            this.groupBoxSplitOptions.TabIndex = 0;
            this.groupBoxSplitOptions.TabStop = false;
            this.groupBoxSplitOptions.Text = "Split options";
            // 
            // groupBoxSubtitleInfo
            // 
            this.groupBoxSubtitleInfo.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxSubtitleInfo.Controls.Add(this.labelCharacters);
            this.groupBoxSubtitleInfo.Controls.Add(this.labelLines);
            this.groupBoxSubtitleInfo.Location = new System.Drawing.Point(311, 13);
            this.groupBoxSubtitleInfo.Name = "groupBoxSubtitleInfo";
            this.groupBoxSubtitleInfo.Size = new System.Drawing.Size(367, 67);
            this.groupBoxSubtitleInfo.TabIndex = 2;
            this.groupBoxSubtitleInfo.TabStop = false;
            this.groupBoxSubtitleInfo.Text = "Subtitle info";
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
            // labelLines
            // 
            this.labelLines.AutoSize = true;
            this.labelLines.Location = new System.Drawing.Point(7, 27);
            this.labelLines.Name = "labelLines";
            this.labelLines.Size = new System.Drawing.Size(93, 13);
            this.labelLines.TabIndex = 0;
            this.labelLines.Text = "Number of lines: X";
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
            this.numericUpDownParts.TabIndex = 1;
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
            this.radioButtonCharacters.TabIndex = 0;
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
            this.groupBoxOutput.Controls.Add(this.buttonOpenOutputFolder);
            this.groupBoxOutput.Controls.Add(this.labelFileName);
            this.groupBoxOutput.Controls.Add(this.textBoxFileName);
            this.groupBoxOutput.Controls.Add(this.comboBoxSubtitleFormats);
            this.groupBoxOutput.Controls.Add(this.labelEncoding);
            this.groupBoxOutput.Controls.Add(this.comboBoxEncoding);
            this.groupBoxOutput.Controls.Add(this.labelOutputFormat);
            this.groupBoxOutput.Controls.Add(this.labelChooseOutputFolder);
            this.groupBoxOutput.Controls.Add(this.buttonChooseFolder);
            this.groupBoxOutput.Controls.Add(this.textBoxOutputFolder);
            this.groupBoxOutput.Location = new System.Drawing.Point(12, 115);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(688, 143);
            this.groupBoxOutput.TabIndex = 1;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "Output";
            // 
            // buttonOpenOutputFolder
            // 
            this.buttonOpenOutputFolder.Location = new System.Drawing.Point(574, 75);
            this.buttonOpenOutputFolder.Name = "buttonOpenOutputFolder";
            this.buttonOpenOutputFolder.Size = new System.Drawing.Size(81, 23);
            this.buttonOpenOutputFolder.TabIndex = 5;
            this.buttonOpenOutputFolder.Text = "Open...";
            this.buttonOpenOutputFolder.UseVisualStyleBackColor = true;
            this.buttonOpenOutputFolder.Click += new System.EventHandler(this.buttonOpenOutputFolder_Click);
            // 
            // labelFileName
            // 
            this.labelFileName.AutoSize = true;
            this.labelFileName.Location = new System.Drawing.Point(6, 16);
            this.labelFileName.Name = "labelFileName";
            this.labelFileName.Size = new System.Drawing.Size(52, 13);
            this.labelFileName.TabIndex = 0;
            this.labelFileName.Text = "File name";
            // 
            // textBoxFileName
            // 
            this.textBoxFileName.Location = new System.Drawing.Point(7, 32);
            this.textBoxFileName.Name = "textBoxFileName";
            this.textBoxFileName.Size = new System.Drawing.Size(529, 20);
            this.textBoxFileName.TabIndex = 1;
            this.textBoxFileName.TextChanged += new System.EventHandler(this.textBoxFileName_TextChanged);
            // 
            // comboBoxSubtitleFormats
            // 
            this.comboBoxSubtitleFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFormats.FormattingEnabled = true;
            this.comboBoxSubtitleFormats.Location = new System.Drawing.Point(51, 107);
            this.comboBoxSubtitleFormats.Name = "comboBoxSubtitleFormats";
            this.comboBoxSubtitleFormats.Size = new System.Drawing.Size(225, 21);
            this.comboBoxSubtitleFormats.TabIndex = 7;
            this.comboBoxSubtitleFormats.SelectedIndexChanged += new System.EventHandler(this.comboBoxSubtitleFormats_SelectedIndexChanged);
            // 
            // labelEncoding
            // 
            this.labelEncoding.AutoSize = true;
            this.labelEncoding.Location = new System.Drawing.Point(336, 110);
            this.labelEncoding.Name = "labelEncoding";
            this.labelEncoding.Size = new System.Drawing.Size(52, 13);
            this.labelEncoding.TabIndex = 8;
            this.labelEncoding.Text = "Encoding";
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.FormattingEnabled = true;
            this.comboBoxEncoding.Location = new System.Drawing.Point(394, 107);
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.Size = new System.Drawing.Size(221, 21);
            this.comboBoxEncoding.TabIndex = 9;
            // 
            // labelOutputFormat
            // 
            this.labelOutputFormat.AutoSize = true;
            this.labelOutputFormat.Location = new System.Drawing.Point(6, 110);
            this.labelOutputFormat.Name = "labelOutputFormat";
            this.labelOutputFormat.Size = new System.Drawing.Size(39, 13);
            this.labelOutputFormat.TabIndex = 6;
            this.labelOutputFormat.Text = "Format";
            // 
            // labelChooseOutputFolder
            // 
            this.labelChooseOutputFolder.AutoSize = true;
            this.labelChooseOutputFolder.Location = new System.Drawing.Point(6, 61);
            this.labelChooseOutputFolder.Name = "labelChooseOutputFolder";
            this.labelChooseOutputFolder.Size = new System.Drawing.Size(105, 13);
            this.labelChooseOutputFolder.TabIndex = 2;
            this.labelChooseOutputFolder.Text = "Choose output folder";
            // 
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.Location = new System.Drawing.Point(542, 75);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(26, 23);
            this.buttonChooseFolder.TabIndex = 4;
            this.buttonChooseFolder.Text = "...";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            this.buttonChooseFolder.Click += new System.EventHandler(this.buttonChooseFolder_Click);
            // 
            // textBoxOutputFolder
            // 
            this.textBoxOutputFolder.Location = new System.Drawing.Point(7, 77);
            this.textBoxOutputFolder.Name = "textBoxOutputFolder";
            this.textBoxOutputFolder.Size = new System.Drawing.Size(529, 20);
            this.textBoxOutputFolder.TabIndex = 3;
            this.textBoxOutputFolder.TextChanged += new System.EventHandler(this.textBoxOutputFolder_TextChanged);
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.listViewParts);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 264);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(688, 254);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
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
            this.listViewParts.Size = new System.Drawing.Size(682, 235);
            this.listViewParts.TabIndex = 0;
            this.listViewParts.UseCompatibleStateImageBehavior = false;
            this.listViewParts.View = System.Windows.Forms.View.Details;
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
            // columnHeaderFileName
            // 
            this.columnHeaderFileName.Text = "File name";
            this.columnHeaderFileName.Width = 463;
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(625, 524);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonSplit
            // 
            this.buttonSplit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonSplit.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonSplit.Location = new System.Drawing.Point(544, 524);
            this.buttonSplit.Name = "buttonSplit";
            this.buttonSplit.Size = new System.Drawing.Size(75, 23);
            this.buttonSplit.TabIndex = 4;
            this.buttonSplit.Text = "&Split";
            this.buttonSplit.UseVisualStyleBackColor = true;
            this.buttonSplit.Click += new System.EventHandler(this.buttonSplit_Click);
            // 
            // buttonBasic
            // 
            this.buttonBasic.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonBasic.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonBasic.Location = new System.Drawing.Point(440, 524);
            this.buttonBasic.Name = "buttonBasic";
            this.buttonBasic.Size = new System.Drawing.Size(98, 23);
            this.buttonBasic.TabIndex = 3;
            this.buttonBasic.Text = "&Basic";
            this.buttonBasic.UseVisualStyleBackColor = true;
            this.buttonBasic.Click += new System.EventHandler(this.buttonBasic_Click);
            // 
            // radioButtonTime
            // 
            this.radioButtonTime.AutoSize = true;
            this.radioButtonTime.Location = new System.Drawing.Point(15, 66);
            this.radioButtonTime.Name = "radioButtonTime";
            this.radioButtonTime.Size = new System.Drawing.Size(48, 17);
            this.radioButtonTime.TabIndex = 0;
            this.radioButtonTime.Text = "Time";
            this.radioButtonTime.UseVisualStyleBackColor = true;
            // 
            // Split
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(712, 556);
            this.Controls.Add(this.buttonBasic);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonSplit);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.groupBoxOutput);
            this.Controls.Add(this.groupBoxSplitOptions);
            this.KeyPreview = true;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.MinimumSize = new System.Drawing.Size(640, 420);
            this.Name = "Split";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Split";
            this.Shown += new System.EventHandler(this.Split_Shown);
            this.ResizeEnd += new System.EventHandler(this.Split_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.Split_KeyDown);
            this.groupBoxSplitOptions.ResumeLayout(false);
            this.groupBoxSplitOptions.PerformLayout();
            this.groupBoxSubtitleInfo.ResumeLayout(false);
            this.groupBoxSubtitleInfo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownParts)).EndInit();
            this.groupBoxOutput.ResumeLayout(false);
            this.groupBoxOutput.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBoxSplitOptions;
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
        private System.Windows.Forms.GroupBox groupBoxSubtitleInfo;
        private System.Windows.Forms.Label labelCharacters;
        private System.Windows.Forms.Label labelLines;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFormats;
        private System.Windows.Forms.Label labelEncoding;
        private System.Windows.Forms.Label labelFileName;
        private System.Windows.Forms.TextBox textBoxFileName;
        private System.Windows.Forms.Button buttonOpenOutputFolder;
        private System.Windows.Forms.RadioButton radioButtonTime;
    }
}