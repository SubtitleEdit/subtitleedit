
namespace Nikse.SubtitleEdit.Forms.Assa
{
    sealed partial class ApplyCustomStyles
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
            this.groupBoxApplyTo = new System.Windows.Forms.GroupBox();
            this.labelAdvancedSelection = new System.Windows.Forms.Label();
            this.buttonAdvancedSelection = new System.Windows.Forms.Button();
            this.radioButtonAdvancedSelection = new System.Windows.Forms.RadioButton();
            this.radioButtonAllLines = new System.Windows.Forms.RadioButton();
            this.radioButtonSelectedLines = new System.Windows.Forms.RadioButton();
            this.groupBoxPreview = new System.Windows.Forms.GroupBox();
            this.pictureBoxPreview = new System.Windows.Forms.PictureBox();
            this.labelOverrideTags = new System.Windows.Forms.Label();
            this.buttonOK = new System.Windows.Forms.Button();
            this.buttonCancel = new System.Windows.Forms.Button();
            this.buttonHistory = new System.Windows.Forms.Button();
            this.buttonTogglePreview = new System.Windows.Forms.Button();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.seTextBox1 = new Nikse.SubtitleEdit.Controls.SETextBox();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.groupBoxApplyTo.SuspendLayout();
            this.groupBoxPreview.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).BeginInit();
            this.SuspendLayout();
            // 
            // groupBoxApplyTo
            // 
            this.groupBoxApplyTo.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxApplyTo.Controls.Add(this.labelAdvancedSelection);
            this.groupBoxApplyTo.Controls.Add(this.buttonAdvancedSelection);
            this.groupBoxApplyTo.Controls.Add(this.radioButtonAdvancedSelection);
            this.groupBoxApplyTo.Controls.Add(this.radioButtonAllLines);
            this.groupBoxApplyTo.Controls.Add(this.radioButtonSelectedLines);
            this.groupBoxApplyTo.Location = new System.Drawing.Point(540, 40);
            this.groupBoxApplyTo.Name = "groupBoxApplyTo";
            this.groupBoxApplyTo.Size = new System.Drawing.Size(246, 156);
            this.groupBoxApplyTo.TabIndex = 1;
            this.groupBoxApplyTo.TabStop = false;
            this.groupBoxApplyTo.Text = "Apply to";
            // 
            // labelAdvancedSelection
            // 
            this.labelAdvancedSelection.AutoSize = true;
            this.labelAdvancedSelection.Location = new System.Drawing.Point(24, 87);
            this.labelAdvancedSelection.Name = "labelAdvancedSelection";
            this.labelAdvancedSelection.Size = new System.Drawing.Size(122, 13);
            this.labelAdvancedSelection.TabIndex = 7;
            this.labelAdvancedSelection.Text = "labelAdvancedSelection";
            // 
            // buttonAdvancedSelection
            // 
            this.buttonAdvancedSelection.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdvancedSelection.Location = new System.Drawing.Point(132, 64);
            this.buttonAdvancedSelection.Name = "buttonAdvancedSelection";
            this.buttonAdvancedSelection.Size = new System.Drawing.Size(29, 23);
            this.buttonAdvancedSelection.TabIndex = 7;
            this.buttonAdvancedSelection.Text = "...";
            this.buttonAdvancedSelection.UseVisualStyleBackColor = true;
            this.buttonAdvancedSelection.Click += new System.EventHandler(this.buttonAdvancedSelection_Click);
            // 
            // radioButtonAdvancedSelection
            // 
            this.radioButtonAdvancedSelection.AutoSize = true;
            this.radioButtonAdvancedSelection.Location = new System.Drawing.Point(7, 67);
            this.radioButtonAdvancedSelection.Name = "radioButtonAdvancedSelection";
            this.radioButtonAdvancedSelection.Size = new System.Drawing.Size(119, 17);
            this.radioButtonAdvancedSelection.TabIndex = 2;
            this.radioButtonAdvancedSelection.TabStop = true;
            this.radioButtonAdvancedSelection.Text = "Advanced selection";
            this.radioButtonAdvancedSelection.UseVisualStyleBackColor = true;
            this.radioButtonAdvancedSelection.CheckedChanged += new System.EventHandler(this.radioButtonAdvancedSelection_CheckedChanged);
            // 
            // radioButtonAllLines
            // 
            this.radioButtonAllLines.AutoSize = true;
            this.radioButtonAllLines.Location = new System.Drawing.Point(7, 44);
            this.radioButtonAllLines.Name = "radioButtonAllLines";
            this.radioButtonAllLines.Size = new System.Drawing.Size(60, 17);
            this.radioButtonAllLines.TabIndex = 1;
            this.radioButtonAllLines.TabStop = true;
            this.radioButtonAllLines.Text = "All lines";
            this.radioButtonAllLines.UseVisualStyleBackColor = true;
            // 
            // radioButtonSelectedLines
            // 
            this.radioButtonSelectedLines.AutoSize = true;
            this.radioButtonSelectedLines.Location = new System.Drawing.Point(7, 20);
            this.radioButtonSelectedLines.Name = "radioButtonSelectedLines";
            this.radioButtonSelectedLines.Size = new System.Drawing.Size(102, 17);
            this.radioButtonSelectedLines.TabIndex = 0;
            this.radioButtonSelectedLines.TabStop = true;
            this.radioButtonSelectedLines.Text = "Selected lines: x";
            this.radioButtonSelectedLines.UseVisualStyleBackColor = true;
            // 
            // groupBoxPreview
            // 
            this.groupBoxPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBoxPreview.Controls.Add(this.pictureBoxPreview);
            this.groupBoxPreview.Location = new System.Drawing.Point(12, 211);
            this.groupBoxPreview.Name = "groupBoxPreview";
            this.groupBoxPreview.Size = new System.Drawing.Size(774, 0);
            this.groupBoxPreview.TabIndex = 2;
            this.groupBoxPreview.TabStop = false;
            this.groupBoxPreview.Text = "Preview";
            // 
            // pictureBoxPreview
            // 
            this.pictureBoxPreview.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pictureBoxPreview.Location = new System.Drawing.Point(3, 16);
            this.pictureBoxPreview.Name = "pictureBoxPreview";
            this.pictureBoxPreview.Size = new System.Drawing.Size(768, 0);
            this.pictureBoxPreview.TabIndex = 0;
            this.pictureBoxPreview.TabStop = false;
            // 
            // labelOverrideTags
            // 
            this.labelOverrideTags.AutoSize = true;
            this.labelOverrideTags.Location = new System.Drawing.Point(13, 21);
            this.labelOverrideTags.Name = "labelOverrideTags";
            this.labelOverrideTags.Size = new System.Drawing.Size(71, 13);
            this.labelOverrideTags.TabIndex = 3;
            this.labelOverrideTags.Text = "Tags to apply";
            // 
            // buttonOK
            // 
            this.buttonOK.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOK.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonOK.Location = new System.Drawing.Point(630, 216);
            this.buttonOK.Name = "buttonOK";
            this.buttonOK.Size = new System.Drawing.Size(75, 23);
            this.buttonOK.TabIndex = 4;
            this.buttonOK.Text = "&OK";
            this.buttonOK.UseVisualStyleBackColor = true;
            this.buttonOK.Click += new System.EventHandler(this.buttonOK_Click);
            // 
            // buttonCancel
            // 
            this.buttonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.buttonCancel.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonCancel.Location = new System.Drawing.Point(711, 216);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new System.Drawing.Size(75, 23);
            this.buttonCancel.TabIndex = 5;
            this.buttonCancel.Text = "C&ancel";
            this.buttonCancel.UseVisualStyleBackColor = true;
            this.buttonCancel.Click += new System.EventHandler(this.buttonCancel_Click);
            // 
            // buttonHistory
            // 
            this.buttonHistory.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonHistory.Location = new System.Drawing.Point(415, 11);
            this.buttonHistory.Name = "buttonHistory";
            this.buttonHistory.Size = new System.Drawing.Size(119, 23);
            this.buttonHistory.TabIndex = 6;
            this.buttonHistory.Text = "History";
            this.buttonHistory.UseVisualStyleBackColor = true;
            this.buttonHistory.Click += new System.EventHandler(this.buttonHistory_Click);
            // 
            // buttonTogglePreview
            // 
            this.buttonTogglePreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonTogglePreview.ImeMode = System.Windows.Forms.ImeMode.NoControl;
            this.buttonTogglePreview.Location = new System.Drawing.Point(481, 216);
            this.buttonTogglePreview.Name = "buttonTogglePreview";
            this.buttonTogglePreview.Size = new System.Drawing.Size(140, 23);
            this.buttonTogglePreview.TabIndex = 7;
            this.buttonTogglePreview.Text = "Show preview";
            this.buttonTogglePreview.UseVisualStyleBackColor = true;
            this.buttonTogglePreview.Click += new System.EventHandler(this.buttonTogglePreview_Click);
            // 
            // timer1
            // 
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // seTextBox1
            // 
            this.seTextBox1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.seTextBox1.BackColor = System.Drawing.SystemColors.WindowFrame;
            this.seTextBox1.ContextMenuStrip = this.contextMenuStrip1;
            this.seTextBox1.CurrentLanguage = "";
            this.seTextBox1.CurrentLineIndex = 0;
            this.seTextBox1.HideSelection = true;
            this.seTextBox1.IsDictionaryDownloaded = true;
            this.seTextBox1.IsSpellCheckerInitialized = false;
            this.seTextBox1.IsSpellCheckRequested = false;
            this.seTextBox1.IsWrongWord = false;
            this.seTextBox1.LanguageChanged = false;
            this.seTextBox1.Location = new System.Drawing.Point(12, 40);
            this.seTextBox1.Multiline = true;
            this.seTextBox1.Name = "seTextBox1";
            this.seTextBox1.Padding = new System.Windows.Forms.Padding(1);
            this.seTextBox1.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.None;
            this.seTextBox1.SelectedText = "";
            this.seTextBox1.SelectionLength = 0;
            this.seTextBox1.SelectionStart = 0;
            this.seTextBox1.Size = new System.Drawing.Size(522, 156);
            this.seTextBox1.TabIndex = 0;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(61, 4);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // ApplyCustomStyles
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(798, 251);
            this.Controls.Add(this.buttonTogglePreview);
            this.Controls.Add(this.buttonHistory);
            this.Controls.Add(this.buttonOK);
            this.Controls.Add(this.buttonCancel);
            this.Controls.Add(this.labelOverrideTags);
            this.Controls.Add(this.groupBoxPreview);
            this.Controls.Add(this.groupBoxApplyTo);
            this.Controls.Add(this.seTextBox1);
            this.KeyPreview = true;
            this.MinimumSize = new System.Drawing.Size(800, 245);
            this.Name = "ApplyCustomStyles";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "ApplyCustomStyles";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ApplyCustomStyles_FormClosing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ApplyCustomStyles_KeyDown);
            this.groupBoxApplyTo.ResumeLayout(false);
            this.groupBoxApplyTo.PerformLayout();
            this.groupBoxPreview.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxPreview)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Controls.SETextBox seTextBox1;
        private System.Windows.Forms.GroupBox groupBoxApplyTo;
        private System.Windows.Forms.GroupBox groupBoxPreview;
        private System.Windows.Forms.Label labelOverrideTags;
        private System.Windows.Forms.Button buttonOK;
        private System.Windows.Forms.Button buttonCancel;
        private System.Windows.Forms.Button buttonHistory;
        private System.Windows.Forms.Label labelAdvancedSelection;
        private System.Windows.Forms.Button buttonAdvancedSelection;
        private System.Windows.Forms.RadioButton radioButtonAdvancedSelection;
        private System.Windows.Forms.RadioButton radioButtonAllLines;
        private System.Windows.Forms.RadioButton radioButtonSelectedLines;
        private System.Windows.Forms.Button buttonTogglePreview;
        private System.Windows.Forms.PictureBox pictureBoxPreview;
        private System.Windows.Forms.Timer timer1;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
    }
}