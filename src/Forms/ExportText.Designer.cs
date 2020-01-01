namespace Nikse.SubtitleEdit.Forms
{
    sealed partial class ExportText
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
            this.textBoxText = new System.Windows.Forms.TextBox();
            this.groupBoxImportOptions = new System.Windows.Forms.GroupBox();
            this.labelEncoding = new System.Windows.Forms.Label();
            this.comboBoxEncoding = new System.Windows.Forms.ComboBox();
            this.checkBoxAddNewlineAfterTimeCodes = new System.Windows.Forms.CheckBox();
            this.checkBoxAddNewlineAfterLineNumber = new System.Windows.Forms.CheckBox();
            this.checkBoxAddNewLine2 = new System.Windows.Forms.CheckBox();
            this.checkBoxAddAfterText = new System.Windows.Forms.CheckBox();
            this.checkBoxShowLineNumbers = new System.Windows.Forms.CheckBox();
            this.groupBoxTimeCodeFormat = new System.Windows.Forms.GroupBox();
            this.comboBoxTimeCodeSeparator = new System.Windows.Forms.ComboBox();
            this.labelTimeCodeSeparator = new System.Windows.Forms.Label();
            this.radioButtonTimeCodeHHMMSSFF = new System.Windows.Forms.RadioButton();
            this.radioButtonTimeCodeMs = new System.Windows.Forms.RadioButton();
            this.radioButtonTimeCodeSrt = new System.Windows.Forms.RadioButton();
            this.groupBoxFormatText = new System.Windows.Forms.GroupBox();
            this.checkBoxRemoveStyling = new System.Windows.Forms.CheckBox();
            this.radioButtonFormatMergeAll = new System.Windows.Forms.RadioButton();
            this.radioButtonFormatNone = new System.Windows.Forms.RadioButton();
            this.radioButtonFormatUnbreak = new System.Windows.Forms.RadioButton();
            this.checkBoxShowTimeCodes = new System.Windows.Forms.CheckBox();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOK = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.labelPreview = new System.Windows.Forms.Label();
            this.groupBoxImportOptions.SuspendLayout();
            this.groupBoxTimeCodeFormat.SuspendLayout();
            this.groupBoxFormatText.SuspendLayout();
            this.SuspendLayout();
            // 
            // textBoxText
            // 
            this.textBoxText.AllowDrop = true;
            this.textBoxText.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxText.Location = new System.Drawing.Point(12, 40);
            this.textBoxText.MaxLength = 0;
            this.textBoxText.Multiline = true;
            this.textBoxText.Name = "textBoxText";
            this.textBoxText.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.textBoxText.Size = new System.Drawing.Size(403, 405);
            this.textBoxText.TabIndex = 3;
            // 
            // groupBoxImportOptions
            // 
            this.groupBoxImportOptions.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxImportOptions.Controls.Add(this.labelEncoding);
            this.groupBoxImportOptions.Controls.Add(this.comboBoxEncoding);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxAddNewlineAfterTimeCodes);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxAddNewlineAfterLineNumber);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxAddNewLine2);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxAddAfterText);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxShowLineNumbers);
            this.groupBoxImportOptions.Controls.Add(this.groupBoxTimeCodeFormat);
            this.groupBoxImportOptions.Controls.Add(this.groupBoxFormatText);
            this.groupBoxImportOptions.Controls.Add(this.checkBoxShowTimeCodes);
            this.groupBoxImportOptions.Location = new System.Drawing.Point(421, 21);
            this.groupBoxImportOptions.Name = "groupBoxImportOptions";
            this.groupBoxImportOptions.Size = new System.Drawing.Size(373, 427);
            this.groupBoxImportOptions.TabIndex = 0;
            this.groupBoxImportOptions.TabStop = false;
            this.groupBoxImportOptions.Text = "Export options";
            // 
            // labelEncoding
            // 
            this.labelEncoding.AutoSize = true;
            this.labelEncoding.Location = new System.Drawing.Point(13, 392);
            this.labelEncoding.Name = "labelEncoding";
            this.labelEncoding.Size = new System.Drawing.Size(52, 13);
            this.labelEncoding.TabIndex = 8;
            this.labelEncoding.Text = "Encoding";
            // 
            // comboBoxEncoding
            // 
            this.comboBoxEncoding.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxEncoding.FormattingEnabled = true;
            this.comboBoxEncoding.Location = new System.Drawing.Point(71, 389);
            this.comboBoxEncoding.Name = "comboBoxEncoding";
            this.comboBoxEncoding.Size = new System.Drawing.Size(221, 21);
            this.comboBoxEncoding.TabIndex = 9;
            // 
            // checkBoxAddNewlineAfterTimeCodes
            // 
            this.checkBoxAddNewlineAfterTimeCodes.AutoSize = true;
            this.checkBoxAddNewlineAfterTimeCodes.Checked = true;
            this.checkBoxAddNewlineAfterTimeCodes.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddNewlineAfterTimeCodes.Location = new System.Drawing.Point(36, 188);
            this.checkBoxAddNewlineAfterTimeCodes.Name = "checkBoxAddNewlineAfterTimeCodes";
            this.checkBoxAddNewlineAfterTimeCodes.Size = new System.Drawing.Size(160, 17);
            this.checkBoxAddNewlineAfterTimeCodes.TabIndex = 4;
            this.checkBoxAddNewlineAfterTimeCodes.Text = "Add new line after time code";
            this.checkBoxAddNewlineAfterTimeCodes.UseVisualStyleBackColor = true;
            this.checkBoxAddNewlineAfterTimeCodes.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // checkBoxAddNewlineAfterLineNumber
            // 
            this.checkBoxAddNewlineAfterLineNumber.AutoSize = true;
            this.checkBoxAddNewlineAfterLineNumber.Checked = true;
            this.checkBoxAddNewlineAfterLineNumber.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddNewlineAfterLineNumber.Location = new System.Drawing.Point(36, 146);
            this.checkBoxAddNewlineAfterLineNumber.Name = "checkBoxAddNewlineAfterLineNumber";
            this.checkBoxAddNewlineAfterLineNumber.Size = new System.Drawing.Size(168, 17);
            this.checkBoxAddNewlineAfterLineNumber.TabIndex = 2;
            this.checkBoxAddNewlineAfterLineNumber.Text = "Add new line after line number";
            this.checkBoxAddNewlineAfterLineNumber.UseVisualStyleBackColor = true;
            this.checkBoxAddNewlineAfterLineNumber.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // checkBoxAddNewLine2
            // 
            this.checkBoxAddNewLine2.AutoSize = true;
            this.checkBoxAddNewLine2.Checked = true;
            this.checkBoxAddNewLine2.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddNewLine2.Location = new System.Drawing.Point(19, 234);
            this.checkBoxAddNewLine2.Name = "checkBoxAddNewLine2";
            this.checkBoxAddNewLine2.Size = new System.Drawing.Size(172, 17);
            this.checkBoxAddNewLine2.TabIndex = 6;
            this.checkBoxAddNewLine2.Text = "Add new line between subtitles";
            this.checkBoxAddNewLine2.UseVisualStyleBackColor = true;
            this.checkBoxAddNewLine2.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // checkBoxAddAfterText
            // 
            this.checkBoxAddAfterText.AutoSize = true;
            this.checkBoxAddAfterText.Checked = true;
            this.checkBoxAddAfterText.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAddAfterText.Location = new System.Drawing.Point(19, 211);
            this.checkBoxAddAfterText.Name = "checkBoxAddAfterText";
            this.checkBoxAddAfterText.Size = new System.Drawing.Size(136, 17);
            this.checkBoxAddAfterText.TabIndex = 5;
            this.checkBoxAddAfterText.Text = "Add new line after texts";
            this.checkBoxAddAfterText.UseVisualStyleBackColor = true;
            this.checkBoxAddAfterText.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // checkBoxShowLineNumbers
            // 
            this.checkBoxShowLineNumbers.AutoSize = true;
            this.checkBoxShowLineNumbers.Location = new System.Drawing.Point(19, 127);
            this.checkBoxShowLineNumbers.Name = "checkBoxShowLineNumbers";
            this.checkBoxShowLineNumbers.Size = new System.Drawing.Size(115, 17);
            this.checkBoxShowLineNumbers.TabIndex = 1;
            this.checkBoxShowLineNumbers.Text = "Show line numbers";
            this.checkBoxShowLineNumbers.UseVisualStyleBackColor = true;
            this.checkBoxShowLineNumbers.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // groupBoxTimeCodeFormat
            // 
            this.groupBoxTimeCodeFormat.Controls.Add(this.comboBoxTimeCodeSeparator);
            this.groupBoxTimeCodeFormat.Controls.Add(this.labelTimeCodeSeparator);
            this.groupBoxTimeCodeFormat.Controls.Add(this.radioButtonTimeCodeHHMMSSFF);
            this.groupBoxTimeCodeFormat.Controls.Add(this.radioButtonTimeCodeMs);
            this.groupBoxTimeCodeFormat.Controls.Add(this.radioButtonTimeCodeSrt);
            this.groupBoxTimeCodeFormat.Location = new System.Drawing.Point(6, 264);
            this.groupBoxTimeCodeFormat.Name = "groupBoxTimeCodeFormat";
            this.groupBoxTimeCodeFormat.Size = new System.Drawing.Size(326, 119);
            this.groupBoxTimeCodeFormat.TabIndex = 7;
            this.groupBoxTimeCodeFormat.TabStop = false;
            this.groupBoxTimeCodeFormat.Text = "Time code format";
            // 
            // comboBoxTimeCodeSeparator
            // 
            this.comboBoxTimeCodeSeparator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxTimeCodeSeparator.FormattingEnabled = true;
            this.comboBoxTimeCodeSeparator.Items.AddRange(new object[] {
            " --> ",
            " - ",
            " "});
            this.comboBoxTimeCodeSeparator.Location = new System.Drawing.Point(121, 90);
            this.comboBoxTimeCodeSeparator.Name = "comboBoxTimeCodeSeparator";
            this.comboBoxTimeCodeSeparator.Size = new System.Drawing.Size(121, 21);
            this.comboBoxTimeCodeSeparator.TabIndex = 4;
            this.comboBoxTimeCodeSeparator.SelectedIndexChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // labelTimeCodeSeparator
            // 
            this.labelTimeCodeSeparator.AutoSize = true;
            this.labelTimeCodeSeparator.Location = new System.Drawing.Point(13, 93);
            this.labelTimeCodeSeparator.Name = "labelTimeCodeSeparator";
            this.labelTimeCodeSeparator.Size = new System.Drawing.Size(104, 13);
            this.labelTimeCodeSeparator.TabIndex = 3;
            this.labelTimeCodeSeparator.Text = "Time code separator";
            // 
            // radioButtonTimeCodeHHMMSSFF
            // 
            this.radioButtonTimeCodeHHMMSSFF.AutoSize = true;
            this.radioButtonTimeCodeHHMMSSFF.Location = new System.Drawing.Point(16, 65);
            this.radioButtonTimeCodeHHMMSSFF.Name = "radioButtonTimeCodeHHMMSSFF";
            this.radioButtonTimeCodeHHMMSSFF.Size = new System.Drawing.Size(226, 17);
            this.radioButtonTimeCodeHHMMSSFF.TabIndex = 2;
            this.radioButtonTimeCodeHHMMSSFF.Text = "HH:MM:SS:FF (00:00:00:01 - 00:00:02:05)";
            this.radioButtonTimeCodeHHMMSSFF.UseVisualStyleBackColor = true;
            this.radioButtonTimeCodeHHMMSSFF.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // radioButtonTimeCodeMs
            // 
            this.radioButtonTimeCodeMs.AutoSize = true;
            this.radioButtonTimeCodeMs.Location = new System.Drawing.Point(16, 42);
            this.radioButtonTimeCodeMs.Name = "radioButtonTimeCodeMs";
            this.radioButtonTimeCodeMs.Size = new System.Drawing.Size(130, 17);
            this.radioButtonTimeCodeMs.TabIndex = 1;
            this.radioButtonTimeCodeMs.Text = "Milliseconds (0 - 2100)";
            this.radioButtonTimeCodeMs.UseVisualStyleBackColor = true;
            this.radioButtonTimeCodeMs.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // radioButtonTimeCodeSrt
            // 
            this.radioButtonTimeCodeSrt.AutoSize = true;
            this.radioButtonTimeCodeSrt.Checked = true;
            this.radioButtonTimeCodeSrt.Location = new System.Drawing.Point(16, 19);
            this.radioButtonTimeCodeSrt.Name = "radioButtonTimeCodeSrt";
            this.radioButtonTimeCodeSrt.Size = new System.Drawing.Size(191, 17);
            this.radioButtonTimeCodeSrt.TabIndex = 0;
            this.radioButtonTimeCodeSrt.TabStop = true;
            this.radioButtonTimeCodeSrt.Text = "Srt (00:00:00.000 --> 00:00:02.100)";
            this.radioButtonTimeCodeSrt.UseVisualStyleBackColor = true;
            this.radioButtonTimeCodeSrt.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // groupBoxFormatText
            // 
            this.groupBoxFormatText.Controls.Add(this.checkBoxRemoveStyling);
            this.groupBoxFormatText.Controls.Add(this.radioButtonFormatMergeAll);
            this.groupBoxFormatText.Controls.Add(this.radioButtonFormatNone);
            this.groupBoxFormatText.Controls.Add(this.radioButtonFormatUnbreak);
            this.groupBoxFormatText.Location = new System.Drawing.Point(6, 19);
            this.groupBoxFormatText.Name = "groupBoxFormatText";
            this.groupBoxFormatText.Size = new System.Drawing.Size(326, 91);
            this.groupBoxFormatText.TabIndex = 0;
            this.groupBoxFormatText.TabStop = false;
            this.groupBoxFormatText.Text = "Format text";
            // 
            // checkBoxRemoveStyling
            // 
            this.checkBoxRemoveStyling.AutoSize = true;
            this.checkBoxRemoveStyling.Checked = true;
            this.checkBoxRemoveStyling.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxRemoveStyling.Location = new System.Drawing.Point(14, 68);
            this.checkBoxRemoveStyling.Name = "checkBoxRemoveStyling";
            this.checkBoxRemoveStyling.Size = new System.Drawing.Size(98, 17);
            this.checkBoxRemoveStyling.TabIndex = 3;
            this.checkBoxRemoveStyling.Text = "Remove styling";
            this.checkBoxRemoveStyling.UseVisualStyleBackColor = true;
            this.checkBoxRemoveStyling.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // radioButtonFormatMergeAll
            // 
            this.radioButtonFormatMergeAll.AutoSize = true;
            this.radioButtonFormatMergeAll.Location = new System.Drawing.Point(159, 19);
            this.radioButtonFormatMergeAll.Name = "radioButtonFormatMergeAll";
            this.radioButtonFormatMergeAll.Size = new System.Drawing.Size(92, 17);
            this.radioButtonFormatMergeAll.TabIndex = 1;
            this.radioButtonFormatMergeAll.Text = "Merge all lines";
            this.radioButtonFormatMergeAll.UseVisualStyleBackColor = true;
            this.radioButtonFormatMergeAll.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // radioButtonFormatNone
            // 
            this.radioButtonFormatNone.AutoSize = true;
            this.radioButtonFormatNone.Checked = true;
            this.radioButtonFormatNone.Location = new System.Drawing.Point(14, 19);
            this.radioButtonFormatNone.Name = "radioButtonFormatNone";
            this.radioButtonFormatNone.Size = new System.Drawing.Size(51, 17);
            this.radioButtonFormatNone.TabIndex = 0;
            this.radioButtonFormatNone.TabStop = true;
            this.radioButtonFormatNone.Text = "None";
            this.radioButtonFormatNone.UseVisualStyleBackColor = true;
            this.radioButtonFormatNone.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // radioButtonFormatUnbreak
            // 
            this.radioButtonFormatUnbreak.AutoSize = true;
            this.radioButtonFormatUnbreak.Location = new System.Drawing.Point(14, 42);
            this.radioButtonFormatUnbreak.Name = "radioButtonFormatUnbreak";
            this.radioButtonFormatUnbreak.Size = new System.Drawing.Size(90, 17);
            this.radioButtonFormatUnbreak.TabIndex = 2;
            this.radioButtonFormatUnbreak.Text = "Unbreak lines";
            this.radioButtonFormatUnbreak.UseVisualStyleBackColor = true;
            this.radioButtonFormatUnbreak.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // checkBoxShowTimeCodes
            // 
            this.checkBoxShowTimeCodes.AutoSize = true;
            this.checkBoxShowTimeCodes.Location = new System.Drawing.Point(19, 169);
            this.checkBoxShowTimeCodes.Name = "checkBoxShowTimeCodes";
            this.checkBoxShowTimeCodes.Size = new System.Drawing.Size(107, 17);
            this.checkBoxShowTimeCodes.TabIndex = 3;
            this.checkBoxShowTimeCodes.Text = "Show time codes";
            this.checkBoxShowTimeCodes.UseVisualStyleBackColor = true;
            this.checkBoxShowTimeCodes.CheckedChanged += new System.EventHandler(this.radioButtonFormatNone_CheckedChanged);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(719, 454);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 2;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(581, 454);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(132, 23);
            this.buttonOK.TabIndex = 1;
            this.buttonOK.Text = "Save as...";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // labelPreview
            // 
            this.labelPreview.AutoSize = true;
            this.labelPreview.Location = new System.Drawing.Point(13, 21);
            this.labelPreview.Name = "labelPreview";
            this.labelPreview.Size = new System.Drawing.Size(45, 13);
            this.labelPreview.TabIndex = 3;
            this.labelPreview.Text = "Preview";
            // 
            // ExportText
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(806, 487);
            this.Controls.Add(this.labelPreview);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.groupBoxImportOptions);
            this.Controls.Add(this.textBoxText);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(800, 520);
            this.Name = "ExportText";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Export plain text";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ExportText_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ExportText_KeyDown);
            this.groupBoxImportOptions.ResumeLayout(false);
            this.groupBoxImportOptions.PerformLayout();
            this.groupBoxTimeCodeFormat.ResumeLayout(false);
            this.groupBoxTimeCodeFormat.PerformLayout();
            this.groupBoxFormatText.ResumeLayout(false);
            this.groupBoxFormatText.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxText;
        private System.Windows.Forms.GroupBox groupBoxImportOptions;
        private System.Windows.Forms.CheckBox checkBoxShowLineNumbers;
        private System.Windows.Forms.GroupBox groupBoxTimeCodeFormat;
        private System.Windows.Forms.RadioButton radioButtonTimeCodeMs;
        private System.Windows.Forms.RadioButton radioButtonTimeCodeSrt;
        private System.Windows.Forms.GroupBox groupBoxFormatText;
        private System.Windows.Forms.RadioButton radioButtonFormatMergeAll;
        private System.Windows.Forms.RadioButton radioButtonFormatNone;
        private System.Windows.Forms.RadioButton radioButtonFormatUnbreak;
        private System.Windows.Forms.CheckBox checkBoxShowTimeCodes;
        private System.Windows.Forms.CheckBox checkBoxAddNewLine2;
        private System.Windows.Forms.CheckBox checkBoxAddAfterText;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.RadioButton radioButtonTimeCodeHHMMSSFF;
        private System.Windows.Forms.CheckBox checkBoxAddNewlineAfterLineNumber;
        private System.Windows.Forms.CheckBox checkBoxAddNewlineAfterTimeCodes;
        private System.Windows.Forms.ComboBox comboBoxTimeCodeSeparator;
        private System.Windows.Forms.Label labelTimeCodeSeparator;
        private System.Windows.Forms.Label labelPreview;
        private System.Windows.Forms.CheckBox checkBoxRemoveStyling;
        private System.Windows.Forms.Label labelEncoding;
        private System.Windows.Forms.ComboBox comboBoxEncoding;
    }
}