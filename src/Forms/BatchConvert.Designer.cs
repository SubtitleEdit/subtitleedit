namespace Nikse.SubtitleEdit.Forms
{
    partial class BatchConvert
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
            this.buttonConvert = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.groupBoxConvertOptions = new System.Windows.Forms.GroupBox();
            this.checkBoxFixItalics = new System.Windows.Forms.CheckBox();
            this.checkBoxFixCasing = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveTextForHI = new System.Windows.Forms.CheckBox();
            this.checkBoxRemoveFormatting = new System.Windows.Forms.CheckBox();
            this.checkBoxBreakLongLines = new System.Windows.Forms.CheckBox();
            this.groupBoxOutput = new System.Windows.Forms.GroupBox();
            this.buttonStyles = new System.Windows.Forms.Button();
            this.checkBoxOverwrite = new System.Windows.Forms.CheckBox();
            this.comboBoxSubtitleFormats = new System.Windows.Forms.ComboBox();
            this.labelEncoding = new System.Windows.Forms.Label();
            this.comboBoxEncoding = new System.Windows.Forms.ComboBox();
            this.labelOutputFormat = new System.Windows.Forms.Label();
            this.labelChooseOutputFolder = new System.Windows.Forms.Label();
            this.buttonChooseFolder = new System.Windows.Forms.Button();
            this.textBoxOutputFolder = new System.Windows.Forms.TextBox();
            this.groupBoxInput = new System.Windows.Forms.GroupBox();
            this.buttonInputBrowse = new System.Windows.Forms.Button();
            this.labelChooseInputFiles = new System.Windows.Forms.Label();
            this.listViewInputFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.buttonOpenOutputFolder = new System.Windows.Forms.Button();
            this.groupBoxConvertOptions.SuspendLayout();
            this.groupBoxOutput.SuspendLayout();
            this.groupBoxInput.SuspendLayout();
            this.SuspendLayout();
            // 
            // buttonConvert
            // 
            this.buttonConvert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonConvert.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonConvert.Location = new System.Drawing.Point(782, 452);
            this.buttonConvert.Name = "buttonConvert";
            this.buttonConvert.Size = new System.Drawing.Size(98, 21);
            this.buttonConvert.TabIndex = 2;
            this.buttonConvert.Text = "&Convert";
            this.buttonConvert.UseVisualStyleBackColor = true;
            this.buttonConvert.Click += new System.EventHandler(this.buttonConvert_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(886, 452);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 21);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "&Done";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // groupBoxConvertOptions
            // 
            this.groupBoxConvertOptions.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxFixItalics);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxFixCasing);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxRemoveTextForHI);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxRemoveFormatting);
            this.groupBoxConvertOptions.Controls.Add(this.checkBoxBreakLongLines);
            this.groupBoxConvertOptions.Location = new System.Drawing.Point(702, 19);
            this.groupBoxConvertOptions.Name = "groupBoxConvertOptions";
            this.groupBoxConvertOptions.Size = new System.Drawing.Size(241, 147);
            this.groupBoxConvertOptions.TabIndex = 9;
            this.groupBoxConvertOptions.TabStop = false;
            this.groupBoxConvertOptions.Text = "Convert options";
            // 
            // checkBoxFixItalics
            // 
            this.checkBoxFixItalics.AutoSize = true;
            this.checkBoxFixItalics.Location = new System.Drawing.Point(16, 47);
            this.checkBoxFixItalics.Name = "checkBoxFixItalics";
            this.checkBoxFixItalics.Size = new System.Drawing.Size(68, 17);
            this.checkBoxFixItalics.TabIndex = 1;
            this.checkBoxFixItalics.Text = "Fix italics";
            this.checkBoxFixItalics.UseVisualStyleBackColor = true;
            // 
            // checkBoxFixCasing
            // 
            this.checkBoxFixCasing.AutoSize = true;
            this.checkBoxFixCasing.Location = new System.Drawing.Point(16, 93);
            this.checkBoxFixCasing.Name = "checkBoxFixCasing";
            this.checkBoxFixCasing.Size = new System.Drawing.Size(95, 17);
            this.checkBoxFixCasing.TabIndex = 3;
            this.checkBoxFixCasing.Text = "Auto-fix casing";
            this.checkBoxFixCasing.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveTextForHI
            // 
            this.checkBoxRemoveTextForHI.AutoSize = true;
            this.checkBoxRemoveTextForHI.Location = new System.Drawing.Point(16, 116);
            this.checkBoxRemoveTextForHI.Name = "checkBoxRemoveTextForHI";
            this.checkBoxRemoveTextForHI.Size = new System.Drawing.Size(115, 17);
            this.checkBoxRemoveTextForHI.TabIndex = 4;
            this.checkBoxRemoveTextForHI.Text = "Remove text for HI";
            this.checkBoxRemoveTextForHI.UseVisualStyleBackColor = true;
            // 
            // checkBoxRemoveFormatting
            // 
            this.checkBoxRemoveFormatting.AutoSize = true;
            this.checkBoxRemoveFormatting.Location = new System.Drawing.Point(16, 70);
            this.checkBoxRemoveFormatting.Name = "checkBoxRemoveFormatting";
            this.checkBoxRemoveFormatting.Size = new System.Drawing.Size(115, 17);
            this.checkBoxRemoveFormatting.TabIndex = 2;
            this.checkBoxRemoveFormatting.Text = "Remove formatting";
            this.checkBoxRemoveFormatting.UseVisualStyleBackColor = true;
            // 
            // checkBoxBreakLongLines
            // 
            this.checkBoxBreakLongLines.AutoSize = true;
            this.checkBoxBreakLongLines.Location = new System.Drawing.Point(16, 24);
            this.checkBoxBreakLongLines.Name = "checkBoxBreakLongLines";
            this.checkBoxBreakLongLines.Size = new System.Drawing.Size(125, 17);
            this.checkBoxBreakLongLines.TabIndex = 0;
            this.checkBoxBreakLongLines.Text = "Auto-break long lines";
            this.checkBoxBreakLongLines.UseVisualStyleBackColor = true;
            // 
            // groupBoxOutput
            // 
            this.groupBoxOutput.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxOutput.Controls.Add(this.buttonStyles);
            this.groupBoxOutput.Controls.Add(this.checkBoxOverwrite);
            this.groupBoxOutput.Controls.Add(this.buttonOpenOutputFolder);
            this.groupBoxOutput.Controls.Add(this.comboBoxSubtitleFormats);
            this.groupBoxOutput.Controls.Add(this.labelEncoding);
            this.groupBoxOutput.Controls.Add(this.comboBoxEncoding);
            this.groupBoxOutput.Controls.Add(this.labelOutputFormat);
            this.groupBoxOutput.Controls.Add(this.groupBoxConvertOptions);
            this.groupBoxOutput.Controls.Add(this.labelChooseOutputFolder);
            this.groupBoxOutput.Controls.Add(this.buttonChooseFolder);
            this.groupBoxOutput.Controls.Add(this.textBoxOutputFolder);
            this.groupBoxOutput.Location = new System.Drawing.Point(12, 267);
            this.groupBoxOutput.Name = "groupBoxOutput";
            this.groupBoxOutput.Size = new System.Drawing.Size(952, 172);
            this.groupBoxOutput.TabIndex = 1;
            this.groupBoxOutput.TabStop = false;
            this.groupBoxOutput.Text = "Output";
            // 
            // buttonStyles
            // 
            this.buttonStyles.Location = new System.Drawing.Point(326, 100);
            this.buttonStyles.Name = "buttonStyles";
            this.buttonStyles.Size = new System.Drawing.Size(116, 23);
            this.buttonStyles.TabIndex = 6;
            this.buttonStyles.Text = "Style...";
            this.buttonStyles.UseVisualStyleBackColor = true;
            this.buttonStyles.Visible = false;
            this.buttonStyles.Click += new System.EventHandler(this.buttonStyles_Click);
            // 
            // checkBoxOverwrite
            // 
            this.checkBoxOverwrite.AutoSize = true;
            this.checkBoxOverwrite.Location = new System.Drawing.Point(13, 67);
            this.checkBoxOverwrite.Name = "checkBoxOverwrite";
            this.checkBoxOverwrite.Size = new System.Drawing.Size(130, 17);
            this.checkBoxOverwrite.TabIndex = 3;
            this.checkBoxOverwrite.Text = "Overwrite exsiting files";
            this.checkBoxOverwrite.UseVisualStyleBackColor = true;
            // 
            // comboBoxSubtitleFormats
            // 
            this.comboBoxSubtitleFormats.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxSubtitleFormats.FormattingEnabled = true;
            this.comboBoxSubtitleFormats.Location = new System.Drawing.Point(80, 102);
            this.comboBoxSubtitleFormats.Name = "comboBoxSubtitleFormats";
            this.comboBoxSubtitleFormats.Size = new System.Drawing.Size(225, 21);
            this.comboBoxSubtitleFormats.TabIndex = 5;
            this.comboBoxSubtitleFormats.SelectedIndexChanged += new System.EventHandler(this.comboBoxSubtitleFormats_SelectedIndexChanged);
            // 
            // labelEncoding
            // 
            this.labelEncoding.AutoSize = true;
            this.labelEncoding.Location = new System.Drawing.Point(10, 137);
            this.labelEncoding.Name = "labelEncoding";
            this.labelEncoding.Size = new System.Drawing.Size(52, 13);
            this.labelEncoding.TabIndex = 7;
            this.labelEncoding.Text = "Encoding";
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.FormattingEnabled = true;
            this.comboBoxEncoding.Location = new System.Drawing.Point(80, 134);
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.Size = new System.Drawing.Size(221, 21);
            this.comboBoxEncoding.TabIndex = 8;
            // 
            // labelOutputFormat
            // 
            this.labelOutputFormat.AutoSize = true;
            this.labelOutputFormat.Location = new System.Drawing.Point(10, 105);
            this.labelOutputFormat.Name = "labelOutputFormat";
            this.labelOutputFormat.Size = new System.Drawing.Size(39, 13);
            this.labelOutputFormat.TabIndex = 4;
            this.labelOutputFormat.Text = "Format";
            // 
            // labelChooseOutputFolder
            // 
            this.labelChooseOutputFolder.AutoSize = true;
            this.labelChooseOutputFolder.Location = new System.Drawing.Point(10, 25);
            this.labelChooseOutputFolder.Name = "labelChooseOutputFolder";
            this.labelChooseOutputFolder.Size = new System.Drawing.Size(105, 13);
            this.labelChooseOutputFolder.TabIndex = 2;
            this.labelChooseOutputFolder.Text = "Choose output folder";
            // 
            // buttonChooseFolder
            // 
            this.buttonChooseFolder.Location = new System.Drawing.Point(494, 39);
            this.buttonChooseFolder.Name = "buttonChooseFolder";
            this.buttonChooseFolder.Size = new System.Drawing.Size(26, 23);
            this.buttonChooseFolder.TabIndex = 1;
            this.buttonChooseFolder.Text = "...";
            this.buttonChooseFolder.UseVisualStyleBackColor = true;
            this.buttonChooseFolder.Click += new System.EventHandler(this.buttonChooseFolder_Click);
            // 
            // textBoxOutputFolder
            // 
            this.textBoxOutputFolder.Location = new System.Drawing.Point(11, 41);
            this.textBoxOutputFolder.Name = "textBoxOutputFolder";
            this.textBoxOutputFolder.Size = new System.Drawing.Size(477, 20);
            this.textBoxOutputFolder.TabIndex = 0;
            // 
            // groupBoxInput
            // 
            this.groupBoxInput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxInput.Controls.Add(this.buttonInputBrowse);
            this.groupBoxInput.Controls.Add(this.labelChooseInputFiles);
            this.groupBoxInput.Controls.Add(this.listViewInputFiles);
            this.groupBoxInput.Location = new System.Drawing.Point(15, 12);
            this.groupBoxInput.Name = "groupBoxInput";
            this.groupBoxInput.Size = new System.Drawing.Size(949, 249);
            this.groupBoxInput.TabIndex = 0;
            this.groupBoxInput.TabStop = false;
            this.groupBoxInput.Text = "Input";
            // 
            // buttonInputBrowse
            // 
            this.buttonInputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInputBrowse.Location = new System.Drawing.Point(914, 41);
            this.buttonInputBrowse.Name = "buttonInputBrowse";
            this.buttonInputBrowse.Size = new System.Drawing.Size(26, 23);
            this.buttonInputBrowse.TabIndex = 5;
            this.buttonInputBrowse.Text = "...";
            this.buttonInputBrowse.UseVisualStyleBackColor = true;
            this.buttonInputBrowse.Click += new System.EventHandler(this.buttonInputBrowse_Click);
            // 
            // labelChooseInputFiles
            // 
            this.labelChooseInputFiles.AutoSize = true;
            this.labelChooseInputFiles.Location = new System.Drawing.Point(5, 25);
            this.labelChooseInputFiles.Name = "labelChooseInputFiles";
            this.labelChooseInputFiles.Size = new System.Drawing.Size(202, 13);
            this.labelChooseInputFiles.TabIndex = 2;
            this.labelChooseInputFiles.Text = "Choose input files (browse or drag-n-drop)";
            // 
            // listViewInputFiles
            // 
            this.listViewInputFiles.AllowDrop = true;
            this.listViewInputFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewInputFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderFName,
            this.columnHeaderSize,
            this.columnHeaderFormat,
            this.columnHeaderStatus});
            this.listViewInputFiles.FullRowSelect = true;
            this.listViewInputFiles.HideSelection = false;
            this.listViewInputFiles.Location = new System.Drawing.Point(6, 41);
            this.listViewInputFiles.Name = "listViewInputFiles";
            this.listViewInputFiles.Size = new System.Drawing.Size(903, 200);
            this.listViewInputFiles.TabIndex = 1;
            this.listViewInputFiles.UseCompatibleStateImageBehavior = false;
            this.listViewInputFiles.View = System.Windows.Forms.View.Details;
            this.listViewInputFiles.DragDrop += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragDrop);
            this.listViewInputFiles.DragEnter += new System.Windows.Forms.DragEventHandler(this.listViewInputFiles_DragEnter);
            // 
            // columnHeaderFName
            // 
            this.columnHeaderFName.Text = "File name";
            this.columnHeaderFName.Width = 500;
            // 
            // columnHeaderSize
            // 
            this.columnHeaderSize.Text = "Size";
            this.columnHeaderSize.Width = 75;
            // 
            // columnHeaderFormat
            // 
            this.columnHeaderFormat.Text = "Format";
            this.columnHeaderFormat.Width = 200;
            // 
            // columnHeaderStatus
            // 
            this.columnHeaderStatus.Text = "Status";
            this.columnHeaderStatus.Width = 124;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            // 
            // buttonOpenOutputFolder
            // 
            this.buttonOpenOutputFolder.Location = new System.Drawing.Point(526, 39);
            this.buttonOpenOutputFolder.Name = "buttonOpenOutputFolder";
            this.buttonOpenOutputFolder.Size = new System.Drawing.Size(102, 23);
            this.buttonOpenOutputFolder.TabIndex = 2;
            this.buttonOpenOutputFolder.Text = "Open...";
            this.buttonOpenOutputFolder.UseVisualStyleBackColor = true;
            this.buttonOpenOutputFolder.Click += new System.EventHandler(this.buttonOpenOutputFolder_Click);
            // 
            // BatchConvert
            // 
            this.AllowDrop = true;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(973, 485);
            this.Controls.Add(this.groupBoxOutput);
            this.Controls.Add(this.groupBoxInput);
            this.Controls.Add(this.buttonConvert);
            this.Controls.Add(this.buttonCancel);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "BatchConvert";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Batch convert";
            this.groupBoxConvertOptions.ResumeLayout(false);
            this.groupBoxConvertOptions.PerformLayout();
            this.groupBoxOutput.ResumeLayout(false);
            this.groupBoxOutput.PerformLayout();
            this.groupBoxInput.ResumeLayout(false);
            this.groupBoxInput.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button buttonConvert;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.GroupBox groupBoxConvertOptions;
        private System.Windows.Forms.CheckBox checkBoxFixCasing;
        private System.Windows.Forms.CheckBox checkBoxRemoveTextForHI;
        private System.Windows.Forms.CheckBox checkBoxRemoveFormatting;
        private System.Windows.Forms.CheckBox checkBoxBreakLongLines;
        private System.Windows.Forms.GroupBox groupBoxOutput;
        private System.Windows.Forms.ComboBox comboBoxSubtitleFormats;
        private System.Windows.Forms.Label labelEncoding;
        private System.Windows.Forms.ComboBox comboBoxEncoding;
        private System.Windows.Forms.Label labelOutputFormat;
        private System.Windows.Forms.Label labelChooseOutputFolder;
        private System.Windows.Forms.Button buttonChooseFolder;
        private System.Windows.Forms.TextBox textBoxOutputFolder;
        private System.Windows.Forms.GroupBox groupBoxInput;
        private System.Windows.Forms.Button buttonInputBrowse;
        private System.Windows.Forms.Label labelChooseInputFiles;
        private System.Windows.Forms.ListView listViewInputFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFName;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ColumnHeader columnHeaderStatus;
        private System.Windows.Forms.CheckBox checkBoxOverwrite;
        private System.Windows.Forms.Button buttonStyles;
        private System.Windows.Forms.CheckBox checkBoxFixItalics;
        private System.Windows.Forms.Button buttonOpenOutputFolder;

    }
}