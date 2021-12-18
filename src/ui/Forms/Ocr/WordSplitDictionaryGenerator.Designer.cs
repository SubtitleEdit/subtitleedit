namespace Nikse.SubtitleEdit.Forms.Ocr
{
    partial class WordSplitDictionaryGenerator
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
            this.comboBoxDictionaries = new System.Windows.Forms.ComboBox();
            this.labelDictionaryLoaded = new System.Windows.Forms.Label();
            this.buttonInputBrowse = new System.Windows.Forms.Button();
            this.listViewInputFiles = new System.Windows.Forms.ListView();
            this.columnHeaderFName = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSize = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderFormat = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.ButtonGenerate = new System.Windows.Forms.Button();
            this.labelStatus = new System.Windows.Forms.Label();
            this.comboBoxMinOccurrences = new System.Windows.Forms.ComboBox();
            this.labelMinOccurSmall = new System.Windows.Forms.Label();
            this.comboBoxMinOccurrencesLongWords = new System.Windows.Forms.ComboBox();
            this.labelMinOccurLarge = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // comboBoxDictionaries
            // 
            this.comboBoxDictionaries.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxDictionaries.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxDictionaries.FormattingEnabled = true;
            this.comboBoxDictionaries.Location = new System.Drawing.Point(725, 12);
            this.comboBoxDictionaries.Name = "comboBoxDictionaries";
            this.comboBoxDictionaries.Size = new System.Drawing.Size(203, 21);
            this.comboBoxDictionaries.TabIndex = 44;
            // 
            // labelDictionaryLoaded
            // 
            this.labelDictionaryLoaded.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelDictionaryLoaded.AutoSize = true;
            this.labelDictionaryLoaded.Location = new System.Drawing.Point(589, 15);
            this.labelDictionaryLoaded.Name = "labelDictionaryLoaded";
            this.labelDictionaryLoaded.Size = new System.Drawing.Size(111, 13);
            this.labelDictionaryLoaded.TabIndex = 43;
            this.labelDictionaryLoaded.Text = "Spell check dictionary";
            // 
            // buttonInputBrowse
            // 
            this.buttonInputBrowse.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonInputBrowse.Location = new System.Drawing.Point(940, 111);
            this.buttonInputBrowse.Name = "buttonInputBrowse";
            this.buttonInputBrowse.Size = new System.Drawing.Size(26, 23);
            this.buttonInputBrowse.TabIndex = 47;
            this.buttonInputBrowse.Text = "...";
            this.buttonInputBrowse.UseVisualStyleBackColor = true;
            this.buttonInputBrowse.Click += new System.EventHandler(this.buttonInputBrowse_Click);
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
            this.columnHeaderFormat});
            this.listViewInputFiles.FullRowSelect = true;
            this.listViewInputFiles.HideSelection = false;
            this.listViewInputFiles.Location = new System.Drawing.Point(12, 111);
            this.listViewInputFiles.Name = "listViewInputFiles";
            this.listViewInputFiles.Size = new System.Drawing.Size(922, 502);
            this.listViewInputFiles.TabIndex = 46;
            this.listViewInputFiles.UseCompatibleStateImageBehavior = false;
            this.listViewInputFiles.View = System.Windows.Forms.View.Details;
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
            // ButtonGenerate
            // 
            this.ButtonGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.ButtonGenerate.Enabled = false;
            this.ButtonGenerate.Location = new System.Drawing.Point(734, 619);
            this.ButtonGenerate.Name = "ButtonGenerate";
            this.ButtonGenerate.Size = new System.Drawing.Size(200, 23);
            this.ButtonGenerate.TabIndex = 48;
            this.ButtonGenerate.Text = "&Generate word split list...";
            this.ButtonGenerate.Click += new System.EventHandler(this.okButton_Click);
            // 
            // labelStatus
            // 
            this.labelStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.labelStatus.AutoSize = true;
            this.labelStatus.Location = new System.Drawing.Point(12, 624);
            this.labelStatus.Name = "labelStatus";
            this.labelStatus.Size = new System.Drawing.Size(59, 13);
            this.labelStatus.TabIndex = 49;
            this.labelStatus.Text = "labelStatus";
            // 
            // comboBoxMinOccurrences
            // 
            this.comboBoxMinOccurrences.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMinOccurrences.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMinOccurrences.FormattingEnabled = true;
            this.comboBoxMinOccurrences.Items.AddRange(new object[] {
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "40",
            "50",
            "60",
            "70",
            "80",
            "90",
            "100",
            "500",
            "1000",
            "10000"});
            this.comboBoxMinOccurrences.Location = new System.Drawing.Point(725, 39);
            this.comboBoxMinOccurrences.Name = "comboBoxMinOccurrences";
            this.comboBoxMinOccurrences.Size = new System.Drawing.Size(203, 21);
            this.comboBoxMinOccurrences.TabIndex = 51;
            // 
            // labelMinOccurSmall
            // 
            this.labelMinOccurSmall.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMinOccurSmall.AutoSize = true;
            this.labelMinOccurSmall.Location = new System.Drawing.Point(576, 42);
            this.labelMinOccurSmall.Name = "labelMinOccurSmall";
            this.labelMinOccurSmall.Size = new System.Drawing.Size(124, 13);
            this.labelMinOccurSmall.TabIndex = 50;
            this.labelMinOccurSmall.Text = "Min occurrences, len < 5";
            // 
            // comboBoxMinOccurrencesLongWords
            // 
            this.comboBoxMinOccurrencesLongWords.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.comboBoxMinOccurrencesLongWords.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxMinOccurrencesLongWords.FormattingEnabled = true;
            this.comboBoxMinOccurrencesLongWords.Items.AddRange(new object[] {
            "2",
            "3",
            "4",
            "5",
            "6",
            "7",
            "8",
            "9",
            "10",
            "11",
            "12",
            "13",
            "14",
            "15",
            "16",
            "17",
            "18",
            "19",
            "20",
            "21",
            "22",
            "23",
            "24",
            "25",
            "26",
            "27",
            "28",
            "29",
            "30",
            "40",
            "50",
            "60",
            "70",
            "80",
            "90",
            "100",
            "500",
            "1000",
            "10000"});
            this.comboBoxMinOccurrencesLongWords.Location = new System.Drawing.Point(725, 66);
            this.comboBoxMinOccurrencesLongWords.Name = "comboBoxMinOccurrencesLongWords";
            this.comboBoxMinOccurrencesLongWords.Size = new System.Drawing.Size(203, 21);
            this.comboBoxMinOccurrencesLongWords.TabIndex = 53;
            // 
            // labelMinOccurLarge
            // 
            this.labelMinOccurLarge.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMinOccurLarge.AutoSize = true;
            this.labelMinOccurLarge.Location = new System.Drawing.Point(570, 69);
            this.labelMinOccurLarge.Name = "labelMinOccurLarge";
            this.labelMinOccurLarge.Size = new System.Drawing.Size(130, 13);
            this.labelMinOccurLarge.TabIndex = 52;
            this.labelMinOccurLarge.Text = "Min occurrences, len >= 5";
            // 
            // WordSplitDictionaryGenerator
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(978, 654);
            this.Controls.Add(this.comboBoxMinOccurrencesLongWords);
            this.Controls.Add(this.labelMinOccurLarge);
            this.Controls.Add(this.comboBoxMinOccurrences);
            this.Controls.Add(this.labelMinOccurSmall);
            this.Controls.Add(this.labelStatus);
            this.Controls.Add(this.ButtonGenerate);
            this.Controls.Add(this.buttonInputBrowse);
            this.Controls.Add(this.listViewInputFiles);
            this.Controls.Add(this.comboBoxDictionaries);
            this.Controls.Add(this.labelDictionaryLoaded);
            this.KeyPreview = true;
            this.Name = "WordSplitDictionaryGenerator";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Word split dictionary generator";
            this.ResizeEnd += new System.EventHandler(this.WordSplitDictionaryGenerator_ResizeEnd);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.WordSplitDictionaryGenerator_KeyDown);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.ComboBox comboBoxDictionaries;
        private System.Windows.Forms.Label labelDictionaryLoaded;
        private System.Windows.Forms.Button buttonInputBrowse;
        private System.Windows.Forms.ListView listViewInputFiles;
        private System.Windows.Forms.ColumnHeader columnHeaderFName;
        private System.Windows.Forms.ColumnHeader columnHeaderSize;
        private System.Windows.Forms.ColumnHeader columnHeaderFormat;
        private System.Windows.Forms.Button ButtonGenerate;
        private System.Windows.Forms.Label labelStatus;
        private System.Windows.Forms.ComboBox comboBoxMinOccurrences;
        private System.Windows.Forms.Label labelMinOccurSmall;
        private System.Windows.Forms.ComboBox comboBoxMinOccurrencesLongWords;
        private System.Windows.Forms.Label labelMinOccurLarge;
    }
}