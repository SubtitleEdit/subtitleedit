
namespace Nikse.SubtitleEdit.Forms.Translate
{
    partial class TranslateViaCopyPaste
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
            this.textBoxLineSeparator = new System.Windows.Forms.TextBox();
            this.labelLineSeparator = new System.Windows.Forms.Label();
            this.checkBoxAutoCopyToClipboard = new System.Windows.Forms.CheckBox();
            this.numericUpDownMaxBytes = new System.Windows.Forms.NumericUpDown();
            this.labelMaxTextSize = new System.Windows.Forms.Label();
            this.progressBarTranslate = new System.Windows.Forms.ProgressBar();
            this.buttonTranslate = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonOk = new System.Windows.Forms.Button();
            this.textBoxLog = new System.Windows.Forms.TextBox();
            this.columnHeaderTarget = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderSource = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeaderNumber = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.listViewTranslate = new System.Windows.Forms.ListView();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxBytes)).BeginInit();
            this.SuspendLayout();
            // 
            // textBoxLineSeparator
            // 
            this.textBoxLineSeparator.Location = new System.Drawing.Point(413, 14);
            this.textBoxLineSeparator.Name = "textBoxLineSeparator";
            this.textBoxLineSeparator.Size = new System.Drawing.Size(41, 20);
            this.textBoxLineSeparator.TabIndex = 4;
            this.textBoxLineSeparator.Text = ".";
            // 
            // labelLineSeparator
            // 
            this.labelLineSeparator.AutoSize = true;
            this.labelLineSeparator.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelLineSeparator.Location = new System.Drawing.Point(338, 18);
            this.labelLineSeparator.Name = "labelLineSeparator";
            this.labelLineSeparator.Size = new System.Drawing.Size(74, 13);
            this.labelLineSeparator.TabIndex = 3;
            this.labelLineSeparator.Text = "Line separator";
            // 
            // checkBoxAutoCopyToClipboard
            // 
            this.checkBoxAutoCopyToClipboard.AutoSize = true;
            this.checkBoxAutoCopyToClipboard.Checked = true;
            this.checkBoxAutoCopyToClipboard.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxAutoCopyToClipboard.Location = new System.Drawing.Point(188, 17);
            this.checkBoxAutoCopyToClipboard.Name = "checkBoxAutoCopyToClipboard";
            this.checkBoxAutoCopyToClipboard.Size = new System.Drawing.Size(132, 17);
            this.checkBoxAutoCopyToClipboard.TabIndex = 2;
            this.checkBoxAutoCopyToClipboard.Text = "Auto-copy to clipboard";
            this.checkBoxAutoCopyToClipboard.UseVisualStyleBackColor = true;
            // 
            // numericUpDownMaxBytes
            // 
            this.numericUpDownMaxBytes.Increment = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMaxBytes.Location = new System.Drawing.Point(105, 15);
            this.numericUpDownMaxBytes.Maximum = new decimal(new int[] {
            100000,
            0,
            0,
            0});
            this.numericUpDownMaxBytes.Minimum = new decimal(new int[] {
            100,
            0,
            0,
            0});
            this.numericUpDownMaxBytes.Name = "numericUpDownMaxBytes";
            this.numericUpDownMaxBytes.Size = new System.Drawing.Size(57, 20);
            this.numericUpDownMaxBytes.TabIndex = 1;
            this.numericUpDownMaxBytes.Value = new decimal(new int[] {
            5000,
            0,
            0,
            0});
            // 
            // labelMaxTextSize
            // 
            this.labelMaxTextSize.AutoSize = true;
            this.labelMaxTextSize.ForeColor = System.Drawing.SystemColors.ControlText;
            this.labelMaxTextSize.Location = new System.Drawing.Point(30, 17);
            this.labelMaxTextSize.Name = "labelMaxTextSize";
            this.labelMaxTextSize.Size = new System.Drawing.Size(68, 13);
            this.labelMaxTextSize.TabIndex = 0;
            this.labelMaxTextSize.Text = "Max text size";
            // 
            // progressBarTranslate
            // 
            this.progressBarTranslate.Location = new System.Drawing.Point(594, 14);
            this.progressBarTranslate.Name = "progressBarTranslate";
            this.progressBarTranslate.Size = new System.Drawing.Size(318, 23);
            this.progressBarTranslate.TabIndex = 6;
            this.progressBarTranslate.Visible = false;
            // 
            // buttonTranslate
            // 
            this.buttonTranslate.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.buttonTranslate.Location = new System.Drawing.Point(475, 13);
            this.buttonTranslate.Name = "buttonTranslate";
            this.buttonTranslate.Size = new System.Drawing.Size(113, 23);
            this.buttonTranslate.TabIndex = 5;
            this.buttonTranslate.Text = "Translate";
            this.buttonTranslate.UseVisualStyleBackColor = true;
            this.buttonTranslate.Click += new System.EventHandler(this.buttonTranslate_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.Location = new System.Drawing.Point(837, 490);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 9;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.Location = new System.Drawing.Point(756, 490);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 8;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            this.buttonOk.Click += new System.EventHandler(this.buttonOk_Click);
            // 
            // textBoxLog
            // 
            this.textBoxLog.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.textBoxLog.Location = new System.Drawing.Point(26, 43);
            this.textBoxLog.Multiline = true;
            this.textBoxLog.Name = "textBoxLog";
            this.textBoxLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.textBoxLog.Size = new System.Drawing.Size(886, 441);
            this.textBoxLog.TabIndex = 20;
            // 
            // columnHeaderTarget
            // 
            this.columnHeaderTarget.Text = "Translation";
            this.columnHeaderTarget.Width = 335;
            // 
            // columnHeaderSource
            // 
            this.columnHeaderSource.Text = "Original";
            this.columnHeaderSource.Width = 350;
            // 
            // columnHeaderNumber
            // 
            this.columnHeaderNumber.Text = "Line #";
            this.columnHeaderNumber.Width = 48;
            // 
            // listViewTranslate
            // 
            this.listViewTranslate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listViewTranslate.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeaderNumber,
            this.columnHeaderSource,
            this.columnHeaderTarget});
            this.listViewTranslate.FullRowSelect = true;
            this.listViewTranslate.GridLines = true;
            this.listViewTranslate.HideSelection = false;
            this.listViewTranslate.Location = new System.Drawing.Point(26, 43);
            this.listViewTranslate.Name = "listViewTranslate";
            this.listViewTranslate.Size = new System.Drawing.Size(886, 441);
            this.listViewTranslate.TabIndex = 7;
            this.listViewTranslate.UseCompatibleStateImageBehavior = false;
            this.listViewTranslate.View = System.Windows.Forms.View.Details;
            // 
            // TranslateViaCopyPaste
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(924, 525);
            this.Controls.Add(this.textBoxLineSeparator);
            this.Controls.Add(this.labelLineSeparator);
            this.Controls.Add(this.checkBoxAutoCopyToClipboard);
            this.Controls.Add(this.numericUpDownMaxBytes);
            this.Controls.Add(this.labelMaxTextSize);
            this.Controls.Add(this.progressBarTranslate);
            this.Controls.Add(this.buttonTranslate);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.buttonOk);
            this.Controls.Add(this.listViewTranslate);
            this.Controls.Add(this.textBoxLog);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(900, 500);
            this.Name = "TranslateViaCopyPaste";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "TranslateViaCopyPaste";
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(MainForm_KeyDown);
            this.Resize += new System.EventHandler(this.MainForm_Resize);
            this.ResizeEnd += new System.EventHandler(this.MainForm_ResizeEnd);
            this.Shown += new System.EventHandler(this.MainForm_Shown);
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDownMaxBytes)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox textBoxLineSeparator;
        private System.Windows.Forms.Label labelLineSeparator;
        private System.Windows.Forms.CheckBox checkBoxAutoCopyToClipboard;
        private System.Windows.Forms.NumericUpDown numericUpDownMaxBytes;
        private System.Windows.Forms.Label labelMaxTextSize;
        private System.Windows.Forms.ProgressBar progressBarTranslate;
        private System.Windows.Forms.Button buttonTranslate;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.TextBox textBoxLog;
        private System.Windows.Forms.ColumnHeader columnHeaderTarget;
        private System.Windows.Forms.ColumnHeader columnHeaderSource;
        private System.Windows.Forms.ColumnHeader columnHeaderNumber;
        private System.Windows.Forms.ListView listViewTranslate;
    }
}